Tasks.CopyFiles({
	name = "Copy all",
	source_folder = "./TestFolder/Source",
	target_folder = "./TestFolder/Target",
	files = {
		"**/*"
	},
	file_exists_action = "overwrite",
	ignore_list = {}
})
