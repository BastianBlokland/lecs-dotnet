.PHONY: clean build test

default: build

clean:
	dotnet clean src/Lecs.sln

build: clean
	dotnet build src/Lecs.sln

test: build
	dotnet test src/Lecs.Tests/Lecs.Tests.csproj
