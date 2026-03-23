FROM alpine:latest

RUN apk add --no-cache \
    gcompat \
    libstdc++ \
    libgcc \
    libintl \
    icu-libs \
    krb5-libs \
    zlib

WORKDIR /app

COPY raw/ /app/

RUN chmod +x /app/sokosolve

ENTRYPOINT ["/app/sokosolve"]
