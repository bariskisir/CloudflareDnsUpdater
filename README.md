# Cloudflare Dns Updater
CloudflareDnsUpdater is a background service running on .NET 8. This service frequently checks domains' A records and update them with the current IP address.
It is designed to be used for hosting applications with dynamic ip address.

## Usage
```sh
docker run -d --name cloudflare-dns-updater --restart always -e "Token=CLOUDFLARE_TOKEN" -e "Domain=test.example.com www.example.com" -e "Delay=5" bariskisir/cloudflarednsupdater
```

### Environment variables
Token is required.  Create "All zones - DNS:Edit" token from cloudflare. [How to](https://support.cloudflare.com/hc/en-us/articles/200167836-Managing-API-Tokens-and-Keys#12345680)

Domains are required.

Delay is optional (minute).

[Dockerhub](https://hub.docker.com/r/bariskisir/cloudflarednsupdater)
