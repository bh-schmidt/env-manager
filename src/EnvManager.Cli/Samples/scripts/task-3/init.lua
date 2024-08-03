Tasks.CopyFiles({
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
