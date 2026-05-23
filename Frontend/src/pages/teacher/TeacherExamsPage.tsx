import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { StudentItem, TeacherAssignment } from '../../types';
import { EXAM_TYPES } from '../../types';
import {
  Alert, Button, Card, CardBody, CardHeader, Input, PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

export function TeacherExamsPage() {
  const [assignments, setAssignments] = useState<TeacherAssignment[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [classId, setClassId] = useState('');
  const [subjectId, setSubjectId] = useState('');
  const [examType, setExamType] = useState('1');
  const [academicYear, setAcademicYear] = useState('2025-2026');
  const [rows, setRows] = useState<Record<string, { marks: string; maxMarks: string; grade: string }>>({});
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    msmsApi.teacher.assignments().then((r) => setAssignments(r.data));
  }, []);

  useEffect(() => {
    if (classId) msmsApi.teacher.classStudents(classId).then((r) => setStudents(r.data));
    else setStudents([]);
  }, [classId]);

  const myClasses = [...new Map(assignments.map((a) => [a.classId, a.className])).entries()];
  const subjectsForClass = assignments.filter((a) => a.classId === classId);
  const classStudents = students.filter((s) => s.classId === classId);

  const save = async () => {
    if (!classId || !subjectId) return;
    setLoading(true);
    setError('');
    try {
      await msmsApi.exams.bulkUpsert({
        classId,
        subjectId,
        examType: Number(examType),
        academicYear,
        lines: classStudents.map((s) => ({
          studentId: s.id,
          marks: Number(rows[s.id]?.marks ?? 0),
          maxMarks: Number(rows[s.id]?.maxMarks ?? 100),
          grade: rows[s.id]?.grade ?? '—',
        })),
      });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader title="Enter Marks" description="Submit exam results for your class" />
      {error && <span className="mb-4 block"><Alert message={error} /></span>}
      <Card className="mb-6">
        <CardHeader title="Exam Details" />
        <CardBody className="grid gap-4 sm:grid-cols-4">
          <Select label="Class" value={classId} onChange={(e) => { setClassId(e.target.value); setSubjectId(''); }}>
            <option value="">— Select —</option>
            {myClasses.map(([id, name]) => <option key={id} value={id}>{name}</option>)}
          </Select>
          <Select label="Subject" value={subjectId} onChange={(e) => setSubjectId(e.target.value)}>
            <option value="">— Select —</option>
            {subjectsForClass.map((a) => <option key={a.subjectId} value={a.subjectId}>{a.subjectName}</option>)}
          </Select>
          <Select label="Exam" value={examType} onChange={(e) => setExamType(e.target.value)}>
            {Object.entries(EXAM_TYPES).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </Select>
          <Input label="Academic Year" value={academicYear} onChange={(e) => setAcademicYear(e.target.value)} />
        </CardBody>
      </Card>
      {classId && subjectId && classStudents.length > 0 && (
        <Card>
          <CardHeader title="Student Marks" action={<Button onClick={save} loading={loading}>Submit All</Button>} />
          <CardBody className="p-0">
            <TableWrap>
              <thead><tr><Th>Student</Th><Th>Marks</Th><Th>Max</Th><Th>Grade</Th></tr></thead>
              <tbody>
                {classStudents.map((s) => (
                  <tr key={s.id}>
                    <Td>{s.firstName} {s.lastName}</Td>
                    <Td>
                      <input
                        className="w-20 rounded border px-2 py-1 text-sm"
                        value={rows[s.id]?.marks ?? ''}
                        onChange={(e) => setRows({ ...rows, [s.id]: { ...rows[s.id], marks: e.target.value, maxMarks: rows[s.id]?.maxMarks ?? '100', grade: rows[s.id]?.grade ?? '' } })}
                      />
                    </Td>
                    <Td>
                      <input
                        className="w-20 rounded border px-2 py-1 text-sm"
                        value={rows[s.id]?.maxMarks ?? '100'}
                        onChange={(e) => setRows({ ...rows, [s.id]: { marks: rows[s.id]?.marks ?? '', maxMarks: e.target.value, grade: rows[s.id]?.grade ?? '' } })}
                      />
                    </Td>
                    <Td>
                      <input
                        className="w-16 rounded border px-2 py-1 text-sm"
                        value={rows[s.id]?.grade ?? ''}
                        onChange={(e) => setRows({ ...rows, [s.id]: { marks: rows[s.id]?.marks ?? '', maxMarks: rows[s.id]?.maxMarks ?? '100', grade: e.target.value } })}
                      />
                    </Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          </CardBody>
        </Card>
      )}
    </>
  );
}
