import clr
import sys
clr.AddReferenceByPartialName("UnityEngine")
clr.AddReferenceByPartialName("Fougerite")
import UnityEngine
import Fougerite

class Test2:
	def TestSharedFunction(self, str1, str2):
		Plugin.Log ("thisisatest", str1 + " " + str2)