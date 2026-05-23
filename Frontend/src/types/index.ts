export type Role = 'Admin' | 'Teacher' | 'Parent' | 'Student';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
  roles: string[];
}

export interface ClassItem {
  id: string;
  name: string;
  gradeLevel: string;
  section: string | null;
  academicYear: string;
  schoolSection: number;
}

export interface ClassStudentSummary {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  photoUrl: string | null;
}

export interface ClassSubjectSummary {
  subjectId: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
}

export interface ClassDetails extends ClassItem {
  studentCount: number;
  subjectCount: number;
  students: ClassStudentSummary[];
  subjects: ClassSubjectSummary[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface SubjectItem {
  id: string;
  name: string;
  code: string;
}

export interface StudentItem {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  classId: string | null;
  status: number;
  photoUrl: string | null;
  gender: number | null;
  className: string | null;
  gradeLevel: string | null;
}

export interface StudentDetails extends StudentItem {
  userId: string;
  dateOfBirth: string | null;
  placeOfBirth: string | null;
  schoolSection: number | null;
  email: string | null;
}

export interface CreateStudentResult {
  studentId: string;
  monthlyFeeAmount: number;
  feeYear: number;
  feeMonth: number;
}

export interface StudentProfile {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string | null;
  placeOfBirth: string | null;
  gender: number | null;
  photoUrl: string | null;
  className: string | null;
  gradeLevel: string | null;
  schoolSection: number | null;
  academicYear: string | null;
  email: string | null;
}

export interface StudentPortal {
  profile: StudentProfile;
  examResults: ExamResult[];
}

export interface AutoPromoteResult {
  promotedCount: number;
  graduatedCount: number;
  messages: string[];
}

export interface ParentItem {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  photoUrl: string | null;
  email: string | null;
  linkedStudentCount: number;
}

export interface ParentDetails extends ParentItem {
  children: {
    id: string;
    studentNumber: string;
    firstName: string;
    lastName: string;
    className: string | null;
  }[];
}

export interface TeacherItem {
  id: string;
  userId: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  photoUrl: string | null;
  email: string | null;
  assignmentCount: number;
}

export interface TeacherDetails extends TeacherItem {
  assignments: {
    subjectId: string;
    subjectName: string;
    classId: string;
    className: string;
  }[];
}

export interface TeacherAssignment {
  teacherId: string;
  teacherName: string;
  subjectId: string;
  subjectName: string;
  classId: string;
  className: string;
}

export interface AttendanceSession {
  id: string;
  classId: string;
  sessionDate: string;
  sessionNumber: number;
  status: string;
  startedByUserId: string;
}

export interface DayAttendanceSummary {
  date: string;
  session1Mark: number | null;
  session2Mark: number | null;
  outcome: string;
}

export interface ExamCatalogItem {
  id: string;
  examType: number;
  name: string;
  academicYear: string;
}

export interface ExamResult {
  id: string;
  studentId: string;
  studentName: string;
  subjectId: string;
  subjectName: string;
  examType: number;
  marks: number;
  maxMarks: number;
  grade: string;
  academicYear: string;
  enteredAtUtc: string;
}

export interface MonthlyFee {
  id: string;
  studentId: string;
  year: number;
  month: number;
  amount: number;
  academicYear: string;
}

export interface Payment {
  id: string;
  studentId: string;
  year: number;
  month: number;
  amount: number;
  paymentDate: string;
  status: number;
  recordedByAdminId: string;
  notes: string | null;
  createdAtUtc: string;
}

export interface FinanceStudentMonth {
  studentId: string;
  studentNumber: string;
  studentName: string;
  className: string | null;
  year: number;
  month: number;
  feeAmount: number;
  paidAmount: number;
  balance: number;
  isPaid: boolean;
}

export interface FinanceMonthSummary {
  year: number;
  month: number;
  totalStudents: number;
  paidStudents: number;
  unpaidStudents: number;
  expectedAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  students: FinanceStudentMonth[];
}

export interface LinkedStudent {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  classId: string | null;
}

export interface GraduatedStudent {
  id: string;
  originalStudentId: string;
  studentNumber: string;
  fullName: string;
  graduationDate: string;
  finalClassId: string;
  academicYear: string;
  archivedAtUtc: string;
}

export const EXAM_TYPES: Record<number, string> = {
  1: 'First Term',
  2: 'Midterm',
  3: 'Third Term',
  4: 'Final',
};

export const PAYMENT_STATUS: Record<number, string> = {
  0: 'Pending',
  1: 'Paid',
  2: 'Partial',
  3: 'Waived',
};

export const STUDENT_STATUS: Record<number, string> = {
  0: 'Active',
  1: 'Withdrawn',
  2: 'Graduated',
};

export const GENDER_LABELS: Record<number, string> = {
  0: 'Male',
  1: 'Female',
};

export const SCHOOL_SECTIONS: Record<number, string> = {
  1: 'Primary (Grades 1–4)',
  2: 'Middle (Grades 5–8)',
  3: 'High (Grades 9–12)',
};
