# import Pkg
# Pkg.add("JSON")
using JSON

# Read and parse the JSON file
deps_file = "deps.json"
output_file = "fedoralinks.txt"
data = JSON.parsefile(deps_file)

# Open output file for writing
open(output_file, "w") do io
    a = 1
    for entry in data
        pname = entry["pname"]
        version = entry["version"]
        println(io, "Source$a: https://www.nuget.org/api/v2/package/$pname/$version")
        a += 1
    end
end

println("Conversion complete. Output saved to fedoralinks.txt")