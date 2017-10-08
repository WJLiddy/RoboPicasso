# Rates an image on a category
import io
from google.cloud import vision
from google.cloud.vision import types

# global singleton var cause classes are for scrubs L M A O


# Inputs -> 
# annotator client (Get once with vision.ImageAnnotatorClient(), then, reuse.)
# raw .png image (see test fn)
# category -> what category was targeted?


# Outputs -> Score (0 ~ 1)
def rate(google_image_annotator_client, content, category):

  # Performace hit? Maybe.
  image = types.Image(content=content)

  # TODO Give the user a lot of leeway - try to detect up to 50 things. Right now gets 10.
  response = google_image_annotator_client.label_detection(image=image)
  labels = response.label_annotations

  guessed_labels = []
  print("LABEL LEN")
  print(len(labels))
  for label in labels:
    if(label.description == category):
      return int(label.score*100)
  return 0
