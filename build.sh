#!/usr/bin/env bash
echo "-----> بدء بناء مشروع المطعم"

# التأكد من وجود مجلد build
mkdir -p build

# بناء المشروع باستخدام dotnet (لأن المشروع ASP.NET Core)
dotnet publish -c Release -o build

echo "-----> تم البناء بنجاح"