.PHONY: clean build test benchmark
default: build

install:
	./ci/install.sh

clean:
	./ci/clean.sh

build: clean
	./ci/build.sh

test: clean
	./ci/test.sh

filter=''
benchmark: clean
	FILTER=$(filter) ./ci/benchmark.sh
