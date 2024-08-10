Steps.fs.copy_all({
	name = "Copy files with ignore list",
	parameters = {
		source_folder = "./TestFolder/Source",
		target_folder = "./TestFolder/Target",
		files = {
			"**/*.json"
		},
		ignore_list = {
			"Inside/file-2.json",
			"./file-3.json"
		},
		file_exists_action = "overwrite",
	}
})
