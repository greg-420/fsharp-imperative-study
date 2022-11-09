from pydriller import Repository
import git, os, re, logging

logging.basicConfig(filename='flagged_commits.log',level=logging.INFO)
# Remove pydriller logs
logging.getLogger("pydriller.repository").setLevel(logging.CRITICAL + 1)

# removed 'error' due to picking up commits that refactor error handling.
# have to match exactly (block 'fault' in 'default')
keywords_regex = [re.compile(r'\bbug\b'), re.compile(r'\bfix\b'), re.compile(r'\bissue\b'), re.compile(r'\bmistake\b'), re.compile(r'\bincorrect\b'), re.compile(r'\bfault\b'), re.compile(r'\bdefect\b'), re.compile(r'\bflaw\b')]

path = './../in/flagged_repos/'

def has_keyword(msg):
    for pattern in keywords_regex:
        if pattern.search(msg) != None:
            return True
    return False

with open ('../in/cleaned_repos.txt', 'r', encoding = 'utf-8') as f:
    repos = f.read().split('\n')
    
n_commits = 0

for repo in repos:
    logging.info("checking %s ...", repo)
    try:
        for commit in Repository(path_to_repo=repo, only_no_merge=True, only_releases=True,only_modifications_with_file_types=['.fs']).traverse_commits():
            msg = commit.msg.casefold()
            if has_keyword(msg):
                n_commits += 1
                logging.info("number of flagged commits so far %d", n_commits)
                repo_path = os.path.join(path, commit.project_name)
                if not os.path.exists(repo_path):
                    os.makedirs(os.path.join(path, commit.project_name))
                    os.makedirs(os.path.join(path, commit.project_name, 'before'))
                    os.makedirs(os.path.join(path, commit.project_name, 'after'))
                before_hash_path = os.path.join(path, commit.project_name, 'before', commit.hash)
                after_hash_path = os.path.join(path, commit.project_name, 'after', commit.hash)
                os.makedirs(before_hash_path)
                os.makedirs(after_hash_path)
                logging.info('created before path: %s', before_hash_path)
                logging.info('created after path: %s', after_hash_path)
                for file in commit.modified_files:
                    if file.filename.endswith('.fs') and file.source_code != None and file.source_code_before != None:
                        with open(os.path.join(before_hash_path, file.filename), 'w', encoding = 'utf-8') as before_writer:
                            before_writer.write(file.source_code_before)
                        with open(os.path.join(after_hash_path, file.filename), 'w', encoding = 'utf-8') as after_writer:  
                            after_writer.write(file.source_code)
    except git.exc.GitCommandError as err: 
        logging.error("ERROR: encountered git error for %s", repo)
    except Exception as err:
        logging.error("ERROR: %s", err)
    logging.info("finished checking %s", repo)