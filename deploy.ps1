# Publishes AlbicelesteWeb.Web and deploys it to Azure App Service (Linux).
# Requires: az login
dotnet publish AlbicelesteWeb.Web -c Release -o publish
Remove-Item app.zip -ErrorAction SilentlyContinue
tar -a -c -f app.zip -C publish .     # tar, not Compress-Archive: Linux needs forward slashes in zip paths
az webapp deploy --resource-group rg-albiceleste --name albiceleste-web-guti8 --src-path app.zip --type zip --clean true