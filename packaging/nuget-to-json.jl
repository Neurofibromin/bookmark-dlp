using JSON
using SHA
using Sockets
using Base64
using HTTP
using XMLDict
using Glob

function process_packages(pkgs_dir)
    package_data = []
    for package in readdir(pkgs_dir)
        package_path = joinpath(pkgs_dir, package)
        if !isdir(package_path)
            continue
        end

        for version in readdir(package_path)
            version_path = joinpath(package_path, version)
            nuspec_files = glob("*.nuspec", version_path)
            if isempty(nuspec_files)
                println("No nuspec file found in: " * version_path)
            else
                nuspec_file = first(nuspec_files) 
            end
            nuspec_string = read(nuspec_file, String)
            nuspec = xml_dict(nuspec_string)

            if !(nuspec isa AbstractDict) || !haskey(nuspec, "package")
                println("Skipping: Invalid XML structure in $(nuspec_file)")
                continue
            end
            
            metadata = nuspec["package"]
            if !(metadata isa AbstractDict) || !haskey(metadata, "metadata")
                println("Skipping: Missing metadata in $(nuspec_file)")
                continue
            end
            
            id = metadata["metadata"]["id"]

            push!(package_data, Dict(
                "pname" => id,
                "version" => version,
            ))
        end
    end

    return package_data
end

function main()
    if length(ARGS) == 0
        println("Usage: julia script.jl <packages directory>")
        exit(1)
    end

    pkgs_dir = ARGS[1]
    result = process_packages(pkgs_dir)
    println(JSON.json(result, 2))
end

main()