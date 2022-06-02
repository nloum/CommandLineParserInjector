using System;

namespace CommandLineParserInjector;

public record CommandLineVerbDescriptor(Type Type, Type? ServiceType);