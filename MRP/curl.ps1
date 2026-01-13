$base = "http://localhost:8080/api"

curl.exe -s -X POST "$base/users/register" -H "Content-Type: application/json" `
  -d '{"username":"demo","password":"demo"}' | Out-Null

$login = curl.exe -s -X POST "$base/users/login" -H "Content-Type: application/json" `
  -d '{"username":"demo","password":"demo"}'
$token = ($login | ConvertFrom-Json).token

curl.exe -s -X POST "$base/media" -H "Content-Type: application/json" `
  -H "Authorization: Bearer $token" `
  -d '{"title":"Matrix","description":"SciFi","mediaType":0,"releaseYear":1999,"genres":["SciFi"],"ageRestriction":16}' | Out-Null

curl.exe -s -X GET "$base/media?title=mat&sortBy=title" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X POST "$base/media/1/rate" -H "Content-Type: application/json" `
  -H "Authorization: Bearer $token" -d '{"stars":5,"comment":"Great"}' | Out-Null

curl.exe -s -X POST "$base/ratings/1/confirm" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X POST "$base/media/1/favorite" -H "Authorization: Bearer $token" | Out-Null

curl.exe -s -X GET "$base/users/demo/profile" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X GET "$base/users/ratings" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X GET "$base/users/favorites" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X GET "$base/users/recommendations" -H "Authorization: Bearer $token" | Out-Null
curl.exe -s -X GET "$base/leaderboard" -H "Authorization: Bearer $token" | Out-Null
