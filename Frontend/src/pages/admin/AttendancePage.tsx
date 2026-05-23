import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { AttendanceSession, ClassItem, StudentItem } from '../../types';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

export function AttendancePage() {
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [sessions, setSessions] = useState<AttendanceSession[]>([]);
  const [classId, setClassId] = useState('');
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [sessionNumber, setSessionNumber] = useState('1');
  const [activeSession, setActiveSession] = useState<AttendanceSession | null>(null);
  const [marks, setMarks] = useState<Record<string, number>>({});
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
    msmsApi.people.listAllStudents().then((r) => setStudents(r.data.items));
  }, []);

  const classStudents = students.filter((s) => s.classId === classId);

  const loadSessions = () => {
    if (!classId) return;
    msmsApi.attendance.listSessions(classId, date).then((r) => setSessions(r.data));
  };

  useEffect(() => { loadSessions(); }, [classId, date]);

  const startSession = async () => {
    if (!classId) return;
    setLoading(true);
    setError('');
    try {
      const { data: id } = await msmsApi.attendance.createSession({
        classId,
        sessionDate: date,
        sessionNumber: Number(sessionNumber),
      });
      setMessage('Session created.');
      loadSessions();
      const session = sessions.find((s) => s.id === id);
      if (session) setActiveSession(session);
      else setActiveSession({ id, classId, sessionDate: date, sessionNumber: Number(sessionNumber), status: 'Open', startedByUserId: '' });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const saveMarks = async () => {
    if (!activeSession) return;
    setLoading(true);
    setError('');
    try {
      const records = classStudents.map((s) => ({
        studentId: s.id,
        mark: marks[s.id] ?? 0,
      }));
      await msmsApi.attendance.upsertRecords(activeSession.id, records);
      setMessage('Attendance saved.');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const closeSession = async () => {
    if (!activeSession) return;
    setLoading(true);
    try {
      await msmsApi.attendance.closeSession(activeSession.id);
      setMessage('Session closed.');
      setActiveSession(null);
      loadSessions();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageHeader title="Attendance" description="Create sessions and record daily attendance" />
      {error && <span className="mb-4 block"><Alert message={error} /></span>}
      {message && <span className="mb-4 block"><Alert message={message} type="success" /></span>}

      <Card className="mb-6">
        <CardHeader title="New Session" />
        <CardBody className="grid gap-4 sm:grid-cols-4">
          <Select label="Class" value={classId} onChange={(e) => setClassId(e.target.value)}>
            <option value="">— Select class —</option>
            {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </Select>
          <Input label="Date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          <Select label="Session" value={sessionNumber} onChange={(e) => setSessionNumber(e.target.value)}>
            <option value="1">Session 1</option>
            <option value="2">Session 2</option>
          </Select>
          <span className="flex items-end">
            <Button onClick={startSession} loading={loading} disabled={!classId}>Start Session</Button>
          </span>
        </CardBody>
      </Card>

      <Card className="mb-6">
        <CardHeader title="Sessions Today" subtitle={sessions.length ? `${sessions.length} session(s)` : undefined} />
        <CardBody className="p-0">
          {sessions.length === 0 ? (
            <EmptyState message="No sessions for this date." />
          ) : (
            <TableWrap>
              <thead><tr><Th>#</Th><Th>Status</Th><Th>Action</Th></tr></thead>
              <tbody>
                {sessions.map((s) => (
                  <tr key={s.id}>
                    <Td>Session {s.sessionNumber}</Td>
                    <Td>{s.status}</Td>
                    <Td>
                      <Button variant="ghost" onClick={() => setActiveSession(s)}>Mark</Button>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>

      {activeSession && classStudents.length > 0 && (
        <Card>
          <CardHeader
            title={`Mark Attendance — Session ${activeSession.sessionNumber}`}
            action={
              <span className="flex gap-2">
                <Button variant="secondary" onClick={closeSession} loading={loading}>Close</Button>
                <Button onClick={saveMarks} loading={loading}>Save</Button>
              </span>
            }
          />
          <CardBody className="p-0">
            <TableWrap>
              <thead><tr><Th>Student</Th><Th>Present</Th><Th>Absent</Th></tr></thead>
              <tbody>
                {classStudents.map((s) => (
                  <tr key={s.id}>
                    <Td className="font-medium">{s.firstName} {s.lastName}</Td>
                    <Td>
                      <input
                        type="radio"
                        name={`mark-${s.id}`}
                        checked={(marks[s.id] ?? 0) === 0}
                        onChange={() => setMarks({ ...marks, [s.id]: 0 })}
                      />
                    </Td>
                    <Td>
                      <input
                        type="radio"
                        name={`mark-${s.id}`}
                        checked={marks[s.id] === 1}
                        onChange={() => setMarks({ ...marks, [s.id]: 1 })}
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
