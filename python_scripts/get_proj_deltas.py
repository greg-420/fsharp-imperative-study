# Generate deltas (after ratio - before ratio) for each project in consolidated csv
import pandas as pd
import csv


def get_delta(vals_bef, vals_aft, key):
    return vals_aft[key] - vals_bef[key]


df_before = pd.read_csv('consolidated_before.csv')
paths = df_before['Path']
df_before = df_before.set_index('Path')
df_after = pd.read_csv('consolidated_after.csv').set_index('Path')
commits = {}

(mucount, loopcount, arrcount, excount, iocount, tokencount) = (' MutableCount', ' LoopCount', ' ArrayCount', ' ExceptionCount', ' IOCount', ' TokenCount')

for path in paths:
    proj_name = path.split('/')[3]
    print(proj_name)
    vals_bef = df_before.loc[path]
    vals_aft = df_after.loc[path.replace('before', 'after')]
    tokens_delta = (get_delta(vals_bef, vals_aft, mucount), get_delta(vals_bef, vals_aft, loopcount), get_delta(
        vals_bef, vals_aft, arrcount), get_delta(vals_bef, vals_aft, excount), get_delta(vals_bef, vals_aft, iocount), get_delta(vals_bef, vals_aft, tokencount))
    if proj_name in commits:
        commits[proj_name].append(tokens_delta)
    else:
        commits[proj_name] = [tokens_delta]

with open('proj_deltas.csv', 'w', newline = '\n') as csvfile:
    csvwriter = csv.writer(csvfile, delimiter=',', quoting=csv.QUOTE_MINIMAL)
    csvwriter.writerow(['proj_commit', 'mutable', 'loop', 'array', 'exception', 'io', 'imp_count', 'imp_ratio', 'tokens'])
    for k in commits.keys():
        counts = commits[k]
        sum_mu = sum_loop = sum_ar = sum_ex = sum_io = imp_ratio = sum_tokens = 0
        for c in counts:
            sum_mu += c[0]
            sum_loop += c[1]
            sum_ar += c[2]
            sum_ex += c[3]
            sum_io += c[4]
            sum_tokens += c[5]
        imp_count = sum_mu + sum_loop + sum_ar + sum_ex + sum_io
        imp_ratio = imp_count / sum_tokens if abs(sum_tokens) > 0 else 0
        csvwriter.writerow([k, sum_mu, sum_loop, sum_ar, sum_ex, sum_io, imp_count, imp_ratio, sum_tokens])
