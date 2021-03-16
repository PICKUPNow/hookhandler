.PHONY: build run \
        start curl curls \
        set-executable

export APP_NAME=hookhandler

# These variables should be supplied by the CI tool,
# but are set to sane defaults when running locally.

export APP_VERSION ?= $(shell id -un)

# Build time required variables

# give IMAGE the fully qualified path name if you plan to push it to a registry
export IMAGE ?= hookhandler
BUILD_TAG ?= build
RELEASE_TAG ?= $(APP_VERSION)

# when running locally, expose the api on this port
LOCAL_PORT = 5000

# ==============
# Docker rules
# ==============

# The build rule is designed to be run locally as well as from the ci tool.
# Builds both build and release-tagged images
# The image created during the build contains the environment and scripts to do pre and post-deployment testing.
build:
	# build the base stage's image, which can run test scripts
	docker build \
		--target build \
		--cache-from $(IMAGE):$(BUILD_TAG) \
		--tag $(IMAGE):$(BUILD_TAG) \
		.

	# now build the release stage's image
	docker build \
		--cache-from $(IMAGE):$(BUILD_TAG) \
		--cache-from $(IMAGE):$(RELEASE_TAG) \
		--tag $(IMAGE):$(RELEASE_TAG) \
		.

run:
	docker run \
		--rm \
		--mount type=bind,source=$(shell pwd)/src/HookHandler.Api/keys,target=//app/keys \
		--publish $(LOCAL_PORT):8000 \
		$(IMAGE):$(RELEASE_TAG)

# ==============
# Convenience Rules for getting a bash command line in each container
# ==============

bash-build:
	docker run --rm -it $(IMAGE):$(BUILD_TAG) bash

bash-release:
	docker run --rm -it --entrypoint bash $(IMAGE):$(RELEASE_TAG)

# ==============
# Local rules
# ==============

# start the app on your box (not in a container)
start:
	dotnet run -p src/HookHandler.Api

# ==============
# Manual Testing rules
# ==============

curl:
	@curl \
		--silent \
		--write-out "\n%{http_code}\n" \
		-X POST \
		-H 'Content-Type: application/json' \
		-H 'Content-Length: 0' \
		http://localhost:$(LOCAL_PORT)/v1

health:
	@curl --write-out "\n%{http_code}\n" http://localhost:$(LOCAL_PORT)/health
