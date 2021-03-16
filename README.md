# hookhandler

Sample code for a PICKUP partner webhook handler. The message sink implementation is left for the partner (we recommend dumping the message to a queue and processing them in a separate application), but this repository contains a sample implementation of verifying the Signature, so that you can authenticate that the webhook was sent by PICKUP.

The PICKUP webhook dispatcher uses a cryptographic signature signed with an asymmetric key to authenticate ourselves. The signature is based on the IETF draft document [Signing HTTP Messages](https://tools.ietf.org/id/draft-richanna-http-message-signatures-00.html). Dealing with the signature is probably going to be the most difficult of the tasks of handling webhooks, so this example application provides an implementation that you can use directly as a starting point, or as a reference. You can process the webhook without validating the signature, but we recommend putting in the effort to validate the signature so that you can ensure the traffic you receive came from PICKUP and was not tampered with in transit.

As far as dealing with the message content, this application provides a very simple implementation that merely writes the message to the log. We recommend writing the message to a queue of your choice so that you can respond very quickly to the webhook dispatcher. Then you can manage the message via another application that is monitoring the queue.

The PICKUP webhook dispatcher, upon receiving an unsuccessful response from your handler, will continue to retry sending the message to you, backing off exponentially up to only retrying every half hour in production. In the sandbox, we max out the retries at one minute to give you a faster feedback loop while setting up and troubleshooting.

## Working with this Repository

Everything should run locally from the `Makefile` as well as from your code editor of choice
(Visual Studio Code or Visual Studio). Debugging locally should work from either tool, and
working outside the containers is great for fast local cycles.

Here's a breakdown of the high points of using the `make` rules.

This repository generates two separate docker images:

1. The build support image, which contains all the automation scripts and the compiled application.
1. The release image, which contains the application to run.

To build the container image:

```sh
make build
```

To run the container image (`ctrl-c` to quit):

```sh
make run
```

When the image is running, to manually test (in another shell!):

```sh
make curl
```

To execute the code natively (i.e. not in a container), you can debug with `F5` or:

```sh
make start
```

## Notes
