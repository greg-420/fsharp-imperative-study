import json
import http.client
import urllib.request

with open('cleaned_repos.txt', 'w', encoding = 'utf-8') as output:
    api_url = 'https://api.github.com/search/repositories?q=language:fsharp+stars:>20&per_page=100&page={0}'
    for i in range(1,9):
        print("starting loop {0}".format(i))
        request = urllib.request.Request(api_url.format(i))
        with urllib.request.urlopen(request) as response:
            data = json.loads(response.read().decode("utf-8"))
            repos = data['items']
            print("length of repo: {0}".format(len(repos)))
            for r in repos:
                output.write(r['html_url'] + '\n')
	