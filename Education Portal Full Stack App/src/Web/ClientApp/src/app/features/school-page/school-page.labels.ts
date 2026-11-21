import {
  AdmissionStatus,
  ClassLevel,
  CurriculumKind,
  FacilityKind,
  PhotoKind,
  SchoolGenderPolicy,
} from '../../web-api-client';

/** Every value of these closed sets, in the order the screens list them. */
export const CURRICULUM_KINDS: readonly CurriculumKind[] = Object.values(CurriculumKind);
export const FACILITY_KINDS: readonly FacilityKind[] = Object.values(FacilityKind);
export const PHOTO_KINDS: readonly PhotoKind[] = Object.values(PhotoKind);
export const GENDER_POLICIES: readonly SchoolGenderPolicy[] = Object.values(SchoolGenderPolicy);
export const ADMISSION_STATUSES: readonly AdmissionStatus[] = Object.values(AdmissionStatus);
export const CLASS_LEVELS: readonly ClassLevel[] = Object.values(ClassLevel);

/** The translated name of a curriculum. The text is chosen at call time, so it follows the page's language. */
export function curriculumLabel(kind: CurriculumKind): string {
  switch (kind) {
    case CurriculumKind.Matric:
      return $localize`:@@schoolPage.curriculumMatric:Matric`;
    case CurriculumKind.Intermediate:
      return $localize`:@@schoolPage.curriculumIntermediate:Intermediate`;
    case CurriculumKind.FederalBoard:
      return $localize`:@@schoolPage.curriculumFederalBoard:Federal board`;
    case CurriculumKind.Cambridge:
      return $localize`:@@schoolPage.curriculumCambridge:Cambridge`;
    case CurriculumKind.InternationalBaccalaureate:
      return $localize`:@@schoolPage.curriculumIb:International Baccalaureate`;
    case CurriculumKind.AmericanDiploma:
      return $localize`:@@schoolPage.curriculumAmerican:American diploma`;
    case CurriculumKind.Montessori:
      return $localize`:@@schoolPage.curriculumMontessori:Montessori`;
    case CurriculumKind.Religious:
      return $localize`:@@schoolPage.curriculumReligious:Religious`;
  }
}

export function facilityLabel(kind: FacilityKind): string {
  switch (kind) {
    case FacilityKind.Transport:
      return $localize`:@@schoolPage.facilityTransport:Transport`;
    case FacilityKind.Hostel:
      return $localize`:@@schoolPage.facilityHostel:Hostel`;
    case FacilityKind.Library:
      return $localize`:@@schoolPage.facilityLibrary:Library`;
    case FacilityKind.ScienceLab:
      return $localize`:@@schoolPage.facilityScienceLab:Science lab`;
    case FacilityKind.ComputerLab:
      return $localize`:@@schoolPage.facilityComputerLab:Computer lab`;
    case FacilityKind.SportsGround:
      return $localize`:@@schoolPage.facilitySportsGround:Sports ground`;
    case FacilityKind.Cafeteria:
      return $localize`:@@schoolPage.facilityCafeteria:Cafeteria`;
    case FacilityKind.PrayerArea:
      return $localize`:@@schoolPage.facilityPrayerArea:Prayer area`;
    case FacilityKind.MedicalRoom:
      return $localize`:@@schoolPage.facilityMedicalRoom:Medical room`;
    case FacilityKind.Cctv:
      return $localize`:@@schoolPage.facilityCctv:CCTV`;
    case FacilityKind.AirConditioning:
      return $localize`:@@schoolPage.facilityAirConditioning:Air conditioning`;
    case FacilityKind.Playground:
      return $localize`:@@schoolPage.facilityPlayground:Playground`;
    case FacilityKind.Auditorium:
      return $localize`:@@schoolPage.facilityAuditorium:Auditorium`;
    case FacilityKind.SwimmingPool:
      return $localize`:@@schoolPage.facilitySwimmingPool:Swimming pool`;
  }
}

export function photoKindLabel(kind: PhotoKind): string {
  switch (kind) {
    case PhotoKind.Campus:
      return $localize`:@@schoolPage.photoCampus:Campus`;
    case PhotoKind.Classroom:
      return $localize`:@@schoolPage.photoClassroom:Classroom`;
    case PhotoKind.Facility:
      return $localize`:@@schoolPage.photoFacility:Facility`;
    case PhotoKind.Event:
      return $localize`:@@schoolPage.photoEvent:Event`;
  }
}

export function genderPolicyLabel(policy: SchoolGenderPolicy): string {
  switch (policy) {
    case SchoolGenderPolicy.BoysOnly:
      return $localize`:@@schoolPage.genderBoys:Boys only`;
    case SchoolGenderPolicy.GirlsOnly:
      return $localize`:@@schoolPage.genderGirls:Girls only`;
    case SchoolGenderPolicy.CoEducation:
      return $localize`:@@schoolPage.genderCoEducation:Co-education`;
  }
}

export function admissionStatusLabel(status: AdmissionStatus): string {
  switch (status) {
    case AdmissionStatus.Open:
      return $localize`:@@schoolPage.admissionOpen:Open`;
    case AdmissionStatus.Closed:
      return $localize`:@@schoolPage.admissionClosed:Closed`;
    case AdmissionStatus.WaitingList:
      return $localize`:@@schoolPage.admissionWaitingList:Waiting list`;
  }
}

export function classLevelLabel(level: ClassLevel): string {
  switch (level) {
    case ClassLevel.Playgroup:
      return $localize`:@@schoolPage.levelPlaygroup:Playgroup`;
    case ClassLevel.Nursery:
      return $localize`:@@schoolPage.levelNursery:Nursery`;
    case ClassLevel.Prep:
      return $localize`:@@schoolPage.levelPrep:Prep`;
    case ClassLevel.Class1:
      return $localize`:@@schoolPage.levelClass1:Class 1`;
    case ClassLevel.Class2:
      return $localize`:@@schoolPage.levelClass2:Class 2`;
    case ClassLevel.Class3:
      return $localize`:@@schoolPage.levelClass3:Class 3`;
    case ClassLevel.Class4:
      return $localize`:@@schoolPage.levelClass4:Class 4`;
    case ClassLevel.Class5:
      return $localize`:@@schoolPage.levelClass5:Class 5`;
    case ClassLevel.Class6:
      return $localize`:@@schoolPage.levelClass6:Class 6`;
    case ClassLevel.Class7:
      return $localize`:@@schoolPage.levelClass7:Class 7`;
    case ClassLevel.Class8:
      return $localize`:@@schoolPage.levelClass8:Class 8`;
    case ClassLevel.Class9:
      return $localize`:@@schoolPage.levelClass9:Class 9`;
    case ClassLevel.Class10:
      return $localize`:@@schoolPage.levelClass10:Class 10`;
    case ClassLevel.FirstYear:
      return $localize`:@@schoolPage.levelFirstYear:First year`;
    case ClassLevel.SecondYear:
      return $localize`:@@schoolPage.levelSecondYear:Second year`;
    case ClassLevel.OLevel:
      return $localize`:@@schoolPage.levelOLevel:O level`;
    case ClassLevel.ALevel:
      return $localize`:@@schoolPage.levelALevel:A level`;
  }
}
