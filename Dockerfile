# ===== „—Õ·… «·»‰«¡ =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# ‰”Œ „·› «·„‘—Ê⁄ „‰ «·„Ã·œ «·›—⁄Ì Restaurant
COPY Restaurant/*.csproj ./Restaurant/
WORKDIR /app/Restaurant
RUN dotnet restore

# ‰”Œ »«ﬁÌ „·›«  «·„‘—Ê⁄
COPY Restaurant/. .
RUN dotnet publish -c Release -o /app/out

# ===== „—Õ·… «· ‘€Ì· =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

#  ⁄—Ì› «·„‰›–
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

#  ‘€Ì· «· ÿ»Ìﬁ («·«”„ „‰ „·› csproj ÂÊ Restaurant)
ENTRYPOINT ["dotnet", "Restaurant.dll"]