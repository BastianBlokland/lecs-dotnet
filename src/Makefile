.PHONY: test build clean

default: build

clean:
	dotnet clean

build: clean
	dotnet build

test: build
	dotnet test Lecs.Tests/Lecs.Tests.csproj
