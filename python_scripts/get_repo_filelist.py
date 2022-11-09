# Turn directories into a text list
import os

with open('flagged_repos_filelist.txt', 'w', encoding = 'utf-8') as out:
    for d in os.listdir():
        out.write(d)
        out.write('\n')
    