import pandas as pd
import os
import csv

dir = '../in/generated_csvs/'
files = os.listdir(dir)
# need to be in alphabetical order since processing after -> before
files.sort()
print('files:', files)
df_before = pd.DataFrame()
df_after = pd.DataFrame()
is_before = True

for file in files:           

    if file.endswith('after.csv'):
        df_after_curr = pd.read_csv(dir + file)
        df_after = pd.concat([df_after, df_after_curr], ignore_index=True)
        
    if file.endswith('before.csv'):
        df_before_curr = pd.read_csv(dir + file)
        df_before = pd.concat([df_before, df_before_curr], ignore_index=True)

df_before.to_csv('consolidated_before.csv',index=False)
df_after.to_csv('consolidated_after.csv',index=False)
