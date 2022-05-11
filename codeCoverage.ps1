dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:"\EnergyLibrary\EnergyLibraryTest\TestResults\11dd4f02-4904-4923-8ee8-6cee6a73f333\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html