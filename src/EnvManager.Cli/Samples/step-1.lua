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
