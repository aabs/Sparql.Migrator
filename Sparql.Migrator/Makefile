PREFIX ?= /usr/local
VERSION = "v1.0.0"
all: install

install:
	mkdir -p $(DESTDIR)$(PREFIX)/bin
	install -m 0755 sparqlmigrator-app-wrapper $(DESTDIR)$(PREFIX)/bin/sparqlmigrator-app

uninstall:
	@$(RM) $(DESTDIR)$(PREFIX)/bin/sparqlmigrator-app
	@docker rmi sparqlmigrator:$(VERSION)
	@docker rmi sparqlmigrator:$(VERSION)

build:
	@rm -rf obj 
	@docker build -t sparqlmigrator:$(VERSION) . \
	&& docker tag -f sparqlmigrator:$(VERSION) sparqlmigrator:$(VERSION)

publish: build
	@docker push sparqlmigrator:$(VERSION) \
	&& docker push sparqlmigrator:$(VERSION)

.PHONY: all install uninstall build publish