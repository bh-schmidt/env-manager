Steps.fs.copy_all({
	name = "Copy all",
	parameters = {
		source_folder = "./TestFolder/Source",
		target_folder = "./TestFolder/Target",
		files = {
			"**/*"
		},
		ignore_list = {},
		file_exists_action = "overwrite",
	}
})

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

Steps.fs.copy_all({
	name = "Copy specified files",
	parameters = {
		source_folder = "./TestFolder/Source",
		target_folder = "./TestFolder/Target",
		files = {
			"Inside/file-2.json",
			"./file-3.json"
		},
		ignore_list = {},
		file_exists_action = "overwrite",
	}
})
