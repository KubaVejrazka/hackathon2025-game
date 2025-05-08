FROM nginx:alpine

WORKDIR /etc/nginx/conf.d
COPY nginx.conf default.conf

WORKDIR /etc/nginx/html
COPY Build/WebGL/WebGL .

RUN mkdir -p ./public-static-void-main-string-args
COPY balancer/out/combined.csv ./public-static-void-main-string-args/balancer-data.csv

CMD ["nginx", "-g", "daemon off;"]