#!/bin/bash -e

docfx_version=2.51.0
docfx=_nuget/docfx.console.$docfx_version/tools/docfx.exe

nuget install -OutputDirectory _nuget docfx.console -Version $docfx_version

rm -rf docs docfx/obj src/Ecoji/obj/xdoc

mkdir -p docs
echo -n ecoji.dev > docs/CNAME

mono $docfx docfx/docfx.json
mono $docfx build -s docfx/docfx.json
