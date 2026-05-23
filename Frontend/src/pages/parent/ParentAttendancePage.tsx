import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import type { DayAttendanceSummary, LinkedStudent } from '../../types';
import { Card, CardBody, CardHeader, EmptyState, Input, PageHeader, Select, TableWrap, Td, Th } from '../../components/ui';

export function ParentAttendancePage() {
  const [children, setChildren] = useState<LinkedStudent[]>([]);
  const [studentId, setStudentId] = useState('');
  const [from, setFrom] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 1);
    return d.toISOString().slice(0, 10);
  });
  const [to, setTo] = useState(new Date().toISOString().slice(0, 10));
  const [rows, setRows] = useState<DayAttendanceSummary[]>([]);

  useEffect(() => {
    msmsApi.parent.children().then((r) => {
      setChildren(r.data);
      if (r.data[0]) setStudentId(r.data[0].id);
    });
  }, []);

  const load = () => {
    if (!studentId) return;
    msmsApi.attendance.daySummaries(studentId, from, to).then((r) => setRows(r.data));
  };

  useEffect(() => { load(); }, [studentId, from, to]);

  return (
    <>
      <PageHeader title="Attendance" description="Daily attendance summary for your child" />
      <Card className="mb-6">
        <CardHeader title="Filters" />
        <CardBody className="grid gap-4 sm:grid-cols-3">
          <Select label="Child" value={studentId} onChange={(e) => setStudentId(e.target.value)}>
            {children.map((c) => (
              <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>
            ))}
          </Select>
          <Input label="From" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
          <Input label="To" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </CardBody>
      </Card>
      <Card>
        <CardBody className="p-0">
          {rows.length === 0 ? (
            <EmptyState message="No attendance records in this range." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Date</Th><Th>Session 1</Th><Th>Session 2</Th><Th>Outcome</Th></tr></thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.date}>
                    <Td>{r.date}</Td>
                    <Td>{r.session1Mark === 0 ? 'Present' : r.session1Mark === 1 ? 'Absent' : '—'}</Td>
                    <Td>{r.session2Mark === 0 ? 'Present' : r.session2Mark === 1 ? 'Absent' : '—'}</Td>
                    <Td>{r.outcome}</Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>
    </>
  );
}
