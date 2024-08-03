Steps.files.copy({
	name = "Copy all",
	source_folder = "./TestFolder/Source",
	target_folder = "./TestFolder/Target",
	files = {
		"**/*"
	},
	file_exists_action = "overwrite",
	ignore_list = {}
})

Steps.files.copy({
	name = "Copy files with ignore list",
	source_folder = "./TestFolder/Source",
	target_folder = "./TestFolder/Target",
	files = {
		"**/*.json"
	},
	file_exists_action = "overwrite",
	ignore_list = {
		"Inside/file-2.json",
		"./file-3.json"
	}
})

Steps.files.copy({
	name = "Copy specified files",
	source_folder = "./TestFolder/Source",
	target_folder = "./TestFolder/Target",
	files = {
		"Inside/file-2.json",
		"./file-3.json"
	},
	file_exists_action = "overwrite",
	ignore_list = {}
})
