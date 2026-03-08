# ===== κ©¥ι΅ ι λ ===== 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 
WORKDIR /app 
 
# λ«¦ κιε ικ¬©νγ ν«Άγ§΅ ι¥ªκ 
COPY *.csproj . 
RUN dotnet restore 
 
# λ«¦  ηο ικιεΆ ν λ ιΆα οη 
COPY . . 
RUN dotnet publish -c Release -o out 
 
# ===== κ©¥ι΅ ιΆ¬δοι ===== 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /app 
 
# λ«¦ ικιεΆ ικ λο΅ 
COPY --from=build /app/out . 
 
# Άγ©οε ικλε¨ 
ENV ASPNETCORE_URLS=http://+:8080 
EXPOSE 8080 
 
# Ά¬δοι ιΆα οη 
