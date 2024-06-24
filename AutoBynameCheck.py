import xml.etree.ElementTree as ET
import re

BynameArray = []
BynameArrayN = []
unuseds = []

NumDict = {}

with open('AutoBynameCheck/BynameAdjective.txt', encoding="utf-16") as my_file:
    for line in my_file:
        BynameArray.append(line)

for i in range(0, len(BynameArray)):
    if BynameArray[i].startswith("	<entry label"):
        num = int(BynameArray[i].split("label=\"")[1].split("\"")[0])
        name = re.sub(u"([\u3040-\u309Fー\u30A0-\u30FF]+)", r"JP\1", BynameArray[i + 1], 1)
        if not "JP" in name:
            # print("JP not found, new line = " + BynameArray[i + 2])
            name = re.sub(u"([\u3040-\u309Fー\u30A0-\u30FF]+)", r"JP\1", BynameArray[i + 2], 1)
        else:
            # print("JP found. Moving")
            pass
        try:
            name = name.split("JP")[1].split("</")[0]
        except IndexError:
            name = "N/A"
        print(str(str(num)  + " - ").replace("00 ", "_0 ").replace("01 ", "_1 ") + name)
        NumDict[num] = name

# print("\nWhat byname would you like to search?")
# byname = int(input(">> "))

nums = []
with open('AutoBynameCheck/BynameList.txt') as my_file:
    for line in my_file:
        nums.append(int(line.split("    - ")[1]))

with open('AutoBynameCheck/BynameUnused.txt') as my_file:
    for line in my_file:
        # unuseds.append(int(line.split('): "')[1].split("\"")[0]))
        print("Append???")

for i in nums:
    try:
        print(NumDict[i])
    except KeyError:
        print("Found unused")
        unuseds.append(i)

for byname in unuseds:
    for i in range(0, len(nums)):
        if byname == nums[i]:
            j = 1
            if NumDict.get(nums[i]) == None:
                print("Byname " + str(byname) + " - Between ", end="")
                try:
                    while (NumDict.get(nums[i - j]) == None):
                        # print("Could not find at " + str(nums[i - j]))
                        j = j + 1
                    old = NumDict.get(nums[i - j])
                    old = old.replace("\\0", "")
                    old = old.replace("&#xE;", "")
                    print(old, end=" and ")
                except IndexError:
                    print("START")
                
                j = 1
                try:
                    while (NumDict.get(nums[i + j]) == None):
                        # print("Could not find at " + str(nums[i + j]))
                        j = j + 1
                    old = NumDict.get(nums[i + j])
                    old = old.replace("\\0", "")
                    old = old.replace("&#xE;", "")
                    print(old + " ()")
                except IndexError:
                    print("END")
            else:
                print("Byname " + str(nums[i]) + " - " + NumDict.get(nums[i]) + " ()")
        else:
            print("Unable to find matching ID")