import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { AttendanceSession, StudentItem, TeacherAssignment } from '../../types';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

export function TeacherAttendancePage() {
  const [assignments, setAssignments] = useState<TeacherAssignment[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [sessions, setSessions] = useState<AttendanceSession[]>([]);
  const [classId, setClassId] = useState('');
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [session, setSession] = useState<AttendanceSession | null>(null);
  const [marks, setMarks] = useState<Record<string, number>>({});
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    msmsApi.teacher.assignments().then((r) => setAssignments(r.data));
  }, []);

  useEffect(() => {
    if (!classId) return;
    msmsApi.teacher.classStudents(classId).then((r) => setStudents(r.data));
    msmsApi.attendance.listSessions(classId, date).then((r) => setSessions(r.data));
  }, [classId, date]);

  const myClasses = [...new Map(assignments.map((a) => [a.classId, a.className])).entries()];

  const save = async () => {
    if (!session) return;
    setLoading(true);
    setError('');
    try {
      await msmsApi.attendance.upsertRecords(
        session.id,
        students.map((s) => ({ studentId: s.id, mark: marks[s.id] ?? 0 }))
      );
      setMessage('Attendance saved.');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader title="Attendance" description="Mark attendance for admin-created sessions" />
      {error && <span className="mb-4 block"><Alert message={error} /></span>}
      {message && <span className="mb-4 block"><Alert message={message} type="success" /></span>}
      <Card className="mb-6">
        <CardHeader title="Select Session" />
        <CardBody className="grid gap-4 sm:grid-cols-3">
          <Select label="Class" value={classId} onChange={(e) => { setClassId(e.target.value); setSession(null); }}>
            <option value="">— Select —</option>
            {myClasses.map(([id, name]) => <option key={id} value={id}>{name}</option>)}
          </Select>
          <Input label="Date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          <Select label="Session" value={session?.id ?? ''} onChange={(e) => setSession(sessions.find((s) => s.id === e.target.value) ?? null)}>
            <option value="">— Select open session —</option>
            {sessions.map((s) => (
              <option key={s.id} value={s.id}>Session {s.sessionNumber} ({s.status})</option>
            ))}
          </Select>
        </CardBody>
      </Card>
      {session && students.length > 0 && (
        <Card>
          <CardHeader title="Students" action={<Button onClick={save} loading={loading}>Save</Button>} />
          <CardBody className="p-0">
            <TableWrap>
              <thead><tr><Th>Student</Th><Th>Present</Th><Th>Absent</Th></tr></thead>
              <tbody>
                {students.map((s) => (
                  <tr key={s.id}>
                    <Td>{s.firstName} {s.lastName}</Td>
                    <Td><input type="radio" name={`m-${s.id}`} checked={(marks[s.id] ?? 0) === 0} onChange={() => setMarks({ ...marks, [s.id]: 0 })} /></Td>
                    <Td><input type="radio" name={`m-${s.id}`} checked={marks[s.id] === 1} onChange={() => setMarks({ ...marks, [s.id]: 1 })} /></Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          </CardBody>
        </Card>
      )}
      {classId && sessions.length === 0 && <EmptyState message="No sessions for this date. Ask admin to create one." />}
    </>
  );
}
