How to use:

* In general, make sure that the file directories point to the correct files
* Follow the steps sequentially. Usually the output for one step is the input for the next

Script/Program: clean_repos.py
Input: 
Output: clean_repos.txt
Comments: Produce a list of F# repositories with >20 stars.

Script/Program: download_flagged_commits.py
Input: clean_repos.txt
Output: All downloaded commits - please point to the directory you want to store them
Comments: Large download, could take hours

Script/Program: get_repo_filelist.py
Input: Directories for the CSV files from the AST tool
Output: Text document of the repos (flagged_repos_filelist.txt)
Comments: You will need the text representation of the directories

Script/Program: ASTAnalyzer (Program.fs)
Input: The downloaded commits from before, flagged_repos_filelist.txt
Output: CSV files containing keyword counts for before and after 
Comments: Long process, could take hours. Run the F# AST tool by navigating to the ASTAnalyzer folder and the command 'dotnet run'. If that does not work, check the dependencies and how to build F# projects

Script/Program: consolidate_imperative_scores.py
Input: CSV files generated from the AST tool
Output: Two consolidated CSV files, consolidated_before.csv and consolidated_after.csv 
Comments: Basically groups the small CSV files into two large ones

Script/Program: get_proj_deltas.py 
Input: consolidated_before.csv and consolidated_after.csv 
Output: proj_deltas.csv
Comments: Outputs CSV file on the project level, with change in imperative tokens and ratios

Script/Program: process_results.ipynb
Input: consolidated_before.csv, consolidated_after.csv and proj_deltas.csv
Output: Statistics and graphs used in the report
Comments: NA