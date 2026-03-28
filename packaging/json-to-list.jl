#!/usr/bin/env -S julia --color=yes --startup-file=no
# import Pkg
# Pkg.add("JSON")
using JSON

function files(deps_file)
    # Read and parse the JSON file
    output_file = "fedoralinks.txt"
    data = JSON.parsefile(deps_file)
    # Open output file for writing
    open(output_file, "w") do io
        a = 1
        for entry in data
            pname = entry["pname"]
            version = entry["version"]
            println(io, "Source$a: https://www.nuget.org/api/v2/package/$pname/$version#$pname.$version.nupkg")
            a += 1
        end
        a = 1
        for entry in data
            pname = entry["pname"]
            version = entry["version"]
            #println(io, "mkdir -p nuget/$pname/$version")
            #println(io, "unzip $pname.$version.nupkg -d nuget/$pname/$version")
            a += 1
        end
    end
    println("Conversion complete. Output saved to fedoralinks.txt")
end

function pipes()
    # Read JSON from standard input
    data = JSON.parse(read(stdin, String))
    a = 1
    for entry in data
        if entry == nothing || isempty(entry)
            continue
        end
        pname = entry["pname"]
        version = entry["version"]
        println("Source$a: https://www.nuget.org/api/v2/package/$pname/$version#$pname.$version.nupkg")
        a += 1
    end
end


function main()
    if length(ARGS) == 0
        pipes()
    else
        deps_file = ARGS[1]
        files(deps_file)
    end
end

main()