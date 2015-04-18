#include "stdafx.h"
#include "OrthoSegment.h"
#include "sa_ipa.h"

COrthographicSegment::COrthographicSegment(EAnnotation index, int master) :
    CDependentSegment(index,master) {
}

CFontTable * COrthographicSegment::NewFontTable() const {
    return new CFontTableANSI;
}

