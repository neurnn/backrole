#!/bin/sh

cd core && chmod +x ./publish.sh && ./publish.sh && cd ..
cd http && chmod +x ./publish.sh && ./publish.sh && cd ..
