# hello world pulumi stack

> this will deploy `app/index.html` to an azure static website
- `pulumi stack init dev`
- requires you to be signed into the azure cli with `az login`
- and also set the azure location with `pulumi config set azure-native:location <value>`