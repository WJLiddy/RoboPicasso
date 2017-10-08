import random

def get_new_prompt():

  labels_file = 'labels/labels.csv'

  with open(labels_file) as l:
    lis = l.read()
    labels = lis.split(",")
    rand = random.randint(0, len(labels))

    return labels[rand]
