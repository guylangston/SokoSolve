clean:
	cd ./test-output/ && rm *

test: clean
	dotnet test

watch-img:
	find -name "*.png" | entr wezterm imgcat /_
