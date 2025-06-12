.PHONY: build install

build:
	hugo --cleanDestinationDir

deploy: build
	rsync -avz --delete public/ csharp@ssh.mini.pw.edu.pl:public_html/