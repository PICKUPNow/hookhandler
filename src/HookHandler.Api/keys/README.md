# keys directory

The `pem` file that exists in this directory is only used for local development. It's the public key from the dev account's `alias/webhooks-dev` kms customer managed key.

When the release container is built, there is no `pem` file loaded into it, so one named `public-key.pem` has to be mounted into the `keys` directory of the container.
