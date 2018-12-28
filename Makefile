.PHONY: clean build test benchmark
default: build

clean:
	dotnet clean src/Lecs.sln

build: clean
	dotnet build src/Lecs.sln

test: build
	dotnet test src/Lecs.Tests/Lecs.Tests.csproj

filter=Lecs.Benchmark.*
benchmark: build
	rm -rf ./artifacts/benchmark
	dotnet run -c Release -p src/Lecs.Benchmark/Lecs.Benchmark.csproj \
	--filter $(filter) --exporters GitHub --artifacts ./artifacts/benchmark
