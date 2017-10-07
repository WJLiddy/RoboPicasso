# Acquire a list of labels recognized by the google images API.
# Uses the files in the bait folder to get the labels.

# Los import statementatos
import io, os, csv, random
from google.cloud import vision
from google.cloud.vision import types


gac_string = "export GOOGLE_APPLICATION_CREDENTIALS=\"" + os.getcwd() + "/google_app_creds.json\""
print ("If you are getting a credentials error, be sure to run:\n " + gac_string)


labels = {}
blacklist = {}

# Helper function that generates labels
def get_labels(path):
	client = vision.ImageAnnotatorClient()

	with io.open(path, 'rb') as image_file:
		content = image_file.read()

	image = types.Image(content=content)

	response = client.label_detection(image=image)
	labels = response.label_annotations

	guessed_labels = []
	for label in labels:
		guessed_labels.append(label.description)
	return guessed_labels

def save_checkpoint():
	writer = csv.writer(open("labels.csv", 'w'))
	row_list = []
	for key, value in labels.items():
		row_list.append(key)
	writer.writerow(sorted(row_list))

# Instantiates a client
client = vision.ImageAnnotatorClient()

# Open up the labels (if there was a checkpoint)
with open('labels.csv', 'r') as f:
	reader = csv.reader(f)
	labels_list = list(reader)

# assure file is not empty.
if(len(labels_list) != 0):
	for label in labels_list[0]:
		labels[label] = True

# Open up the blacklist.
with open('blacklist.csv', 'r') as f:
	reader = csv.reader(f)
	blacklist_list = list(reader)

# assure file is not empty.
if(len(blacklist_list) != 0):
	for label in blacklist_list[0]:
		blacklist[label] = True

while True:
	# pick random file from bait directory
	file = "bait/" + random.choice(os.listdir("bait"))
	print(file)
	new_labels = get_labels(file)
	for l in new_labels:
		if l not in labels and l not in blacklist:
			labels[l] = True
	print(len(labels))
	save_checkpoint()

