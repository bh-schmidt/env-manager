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
