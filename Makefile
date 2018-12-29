.PHONY: clean build test benchmark
default: build

install:
	./ci/install.sh

clean:
	./ci/clean.sh

build: clean
	./ci/build.sh

test: build
	./ci/test.sh

filter=Lecs.Benchmark.*
benchmark: build
	FILTER=$(filter) ./ci/benchmark.sh
