import { api } from './client';

import type {

  AttendanceSession,

  AutoPromoteResult,

  ClassDetails,

  ClassItem,

  CreateStudentResult,

  DayAttendanceSummary,

  ExamCatalogItem,

  ExamResult,

  FinanceMonthSummary,

  GraduatedStudent,

  LinkedStudent,

  MonthlyFee,

  PagedResult,

  ParentItem,

  ParentDetails,

  Payment,

  StudentDetails,

  StudentItem,

  StudentPortal,

  SubjectItem,

  TeacherAssignment,

  TeacherItem,

  TeacherDetails,

  TokenResponse,

} from '../types';



const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5136';



export const msmsApi = {

  auth: {

    login: (email: string, password: string) =>

      api.post<TokenResponse>('/api/auth/login', { email, password }),

    register: (body: Record<string, unknown>) => api.post('/api/auth/register', body),

  },



  school: {

    listClasses: () => api.get<ClassItem[]>('/api/admin/school/classes'),

    getClass: (id: string) => api.get<ClassDetails>(`/api/admin/school/classes/${id}`),

    createClass: (body: object) => api.post<string>('/api/admin/school/classes', body),

    updateClass: (id: string, body: object) => api.put(`/api/admin/school/classes/${id}`, body),

    deleteClass: (id: string) => api.delete(`/api/admin/school/classes/${id}`),

    listSubjects: () => api.get<SubjectItem[]>('/api/admin/school/subjects'),

    createSubject: (body: object) => api.post<string>('/api/admin/school/subjects', body),

    deleteSubject: (id: string) => api.delete(`/api/admin/school/subjects/${id}`),

    assignTeacher: (body: object) => api.post('/api/admin/school/teacher-assignments', body),

    listAssignments: (teacherId?: string) =>

      api.get<TeacherAssignment[]>('/api/admin/school/teacher-assignments', {

        params: teacherId ? { teacherId } : {},

      }),

  },



  people: {

    listStudents: (page = 1, pageSize = 50) =>

      api.get<PagedResult<StudentItem>>('/api/admin/people/students', { params: { page, pageSize } }),

    listAllStudents: () =>

      api.get<PagedResult<StudentItem>>('/api/admin/people/students', { params: { page: 1, pageSize: 0 } }),

    getStudent: (id: string) => api.get<StudentDetails>(`/api/admin/people/students/${id}`),

    deleteStudent: (id: string) => api.delete(`/api/admin/people/students/${id}`),

    feePreview: (classId?: string) =>

      api.get<number>('/api/admin/people/students/fee-preview', {

        params: classId ? { classId } : {},

      }),

    uploadPhoto: (file: File) => {

      const form = new FormData();

      form.append('file', file);

      return api.post<{ url: string }>('/api/admin/people/students/upload-photo', form, {

        headers: { 'Content-Type': 'multipart/form-data' },

      });

    },

    createStudent: (body: object) =>

      api.post<CreateStudentResult>('/api/admin/people/students', body),

    updateStudent: (id: string, body: object) => api.put(`/api/admin/people/students/${id}`, body),

    listParents: () => api.get<ParentItem[]>('/api/admin/people/parents'),

    getParent: (id: string) => api.get<ParentDetails>(`/api/admin/people/parents/${id}`),

    createParent: (body: object) => api.post<string>('/api/admin/people/parents', body),

    updateParent: (id: string, body: object) => api.put(`/api/admin/people/parents/${id}`, body),

    deleteParent: (id: string) => api.delete(`/api/admin/people/parents/${id}`),

    linkParentChild: (parentId: string, studentId: string) =>

      api.post(`/api/admin/people/parents/${parentId}/students/${studentId}`),

    listTeachers: () => api.get<TeacherItem[]>('/api/admin/people/teachers'),

    getTeacher: (id: string) => api.get<TeacherDetails>(`/api/admin/people/teachers/${id}`),

    createTeacher: (body: object) => api.post<string>('/api/admin/people/teachers', body),

    updateTeacher: (id: string, body: object) => api.put(`/api/admin/people/teachers/${id}`, body),

    deleteTeacher: (id: string) => api.delete(`/api/admin/people/teachers/${id}`),

  },



  attendance: {

    createSession: (body: object) => api.post<string>('/api/attendance/sessions', body),

    closeSession: (id: string) => api.post(`/api/attendance/sessions/${id}/close`),

    upsertRecords: (sessionId: string, records: object[]) =>

      api.post(`/api/attendance/sessions/${sessionId}/records`, records),

    listSessions: (classId: string, date?: string) =>

      api.get<AttendanceSession[]>(`/api/attendance/classes/${classId}/sessions`, {

        params: date ? { date } : {},

      }),

    daySummaries: (studentId: string, from: string, to: string) =>

      api.get<DayAttendanceSummary[]>(

        `/api/attendance/students/${studentId}/day-summaries`,

        { params: { from, to } }

      ),

  },



  exams: {

    listCatalog: (academicYear?: string) =>

      api.get<ExamCatalogItem[]>('/api/exam-catalog', { params: academicYear ? { academicYear } : {} }),

    createCatalog: (body: object) => api.post<string>('/api/exam-catalog', body),

    deleteCatalog: (id: string) => api.delete(`/api/exam-catalog/${id}`),

    studentResults: (studentId: string) =>

      api.get<ExamResult[]>(`/api/exam-results/students/${studentId}`),

    classSubjectResults: (classId: string, subjectId: string, examType?: number) =>

      api.get<ExamResult[]>(`/api/exam-results/classes/${classId}/subjects/${subjectId}`, {

        params: examType != null ? { examType } : {},

      }),

    exportCsvUrl: (classId: string, subjectId: string, examType?: number) => {

      const token = localStorage.getItem('msms_access_token');

      const params = new URLSearchParams();

      if (examType != null) params.set('examType', String(examType));

      const qs = params.toString();

      return `${API_URL}/api/exam-results/classes/${classId}/subjects/${subjectId}/export${qs ? `?${qs}` : ''}${token ? '' : ''}`;

    },

    downloadExport: async (classId: string, subjectId: string, examType?: number) => {

      const res = await api.get(

        `/api/exam-results/classes/${classId}/subjects/${subjectId}/export`,

        { params: examType != null ? { examType } : {}, responseType: 'blob' }

      );

      return res.data as Blob;

    },

    bulkUpsert: (body: object) => api.post('/api/exam-results/bulk', body),

  },



  finance: {

    listFees: (studentId?: string) =>

      api.get<MonthlyFee[]>('/api/finance/monthly-fees', { params: studentId ? { studentId } : {} }),

    summary: (year?: number, month?: number) =>

      api.get<FinanceMonthSummary>('/api/finance/summary', { params: { year, month } }),

    upsertFee: (body: object) => api.put<string>('/api/finance/monthly-fees', body),

    deleteFee: (id: string) => api.delete(`/api/finance/monthly-fees/${id}`),

    listPayments: (studentId?: string) =>

      api.get<Payment[]>('/api/finance/payments', { params: studentId ? { studentId } : {} }),

    createPayment: (body: object) => api.post<string>('/api/finance/payments', body),

    updatePayment: (id: string, body: object) => api.put(`/api/finance/payments/${id}`, body),

    deletePayment: (id: string) => api.delete(`/api/finance/payments/${id}`),

    chargeMonth: (studentId: string, year: number, month: number) =>

      api.post<string>('/api/finance/payments/charge-month', { studentId, year, month }),

  },



  lifecycle: {

    promote: (body: object) => api.post('/api/lifecycle/promotions', body),

    autoPromote: (body: { newAcademicYear: string; promotionDate: string }) =>

      api.post<AutoPromoteResult>('/api/lifecycle/promotions/auto', body),

    graduate: (body: object) => api.post('/api/lifecycle/graduations', body),

    listGraduated: () => api.get<GraduatedStudent[]>('/api/lifecycle/graduated-students'),

  },



  parent: {

    children: () => api.get<LinkedStudent[]>('/api/parent/children'),

  },



  student: {

    myPortal: () => api.get<StudentPortal>('/api/student/me/portal'),

    myResults: () => api.get<ExamResult[]>('/api/student/me/exam-results'),

  },



  teacher: {

    assignments: () => api.get<TeacherAssignment[]>('/api/teacher/me/assignments'),

    classStudents: (classId: string) =>

      api.get<StudentItem[]>(`/api/teacher/classes/${classId}/students`),

  },

};

