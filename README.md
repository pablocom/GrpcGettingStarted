# GrpcGettingStarted
Grpc exploration via getting started tutorial

gRPC is a open source high performance Remote Procedure Call (RPC) that is agnostic from any programming language.
It can efficiently connect services in and across data centers with pluggable support for load balancing, tracing, health checking and authentication.

It provides a nice set of tools to make it very performant, 
for example selective message compression. For example, if you are streaming mixed text and images over a single stream (or really any mixed compressible content), 
you can turn off compression for the images. This saves you from compressing already compressed data which won't get any smaller, but will burn up your CPU.

However when trying to achieve loosely coupled services there could be a better option using more Event-Driven Architecture and asynchronous alternatives, using a message bus to integrate different systems.

## Protocol buffers

Defines the contracts that exposes our API, the messages that it handles and returns, and the service method notation that process the request.
As said before there are a lot of built-in features that would make gRPC a nice option if you system requires nice performance and scalability.
It transports through the network in a more light way than plain text JSON so it will save network bandwidth.