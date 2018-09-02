#pragma once
#include "espresso.h"

extern "C" __declspec(dllexport) int * espressonet(int ninputs, int noutputs, int ncubes, int *cover);
