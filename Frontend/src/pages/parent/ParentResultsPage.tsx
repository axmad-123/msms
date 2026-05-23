import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import type { ExamResult, LinkedStudent } from '../../types';
import { EXAM_TYPES } from '../../types';
import { Card, CardBody, EmptyState, PageHeader, Select, TableWrap, Td, Th } from '../../components/ui';

export function ParentResultsPage() {
  const [children, setChildren] = useState<LinkedStudent[]>([]);
  const [studentId, setStudentId] = useState('');
  const [results, setResults] = useState<ExamResult[]>([]);

  useEffect(() => {
    msmsApi.parent.children().then((r) => {
      setChildren(r.data);
      if (r.data[0]) setStudentId(r.data[0].id);
    });
  }, []);

  useEffect(() => {
    if (!studentId) return;
    msmsApi.exams.studentResults(studentId).then((r) => setResults(r.data));
  }, [studentId]);

  return (
    <>
      <PageHeader title="Exam Results" description="Your child's academic performance" />
      <Card className="mb-6">
        <CardBody>
          <Select label="Child" value={studentId} onChange={(e) => setStudentId(e.target.value)}>
            {children.map((c) => (
              <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>
            ))}
          </Select>
        </CardBody>
      </Card>
      <Card>
        <CardBody className="p-0">
          {results.length === 0 ? (
            <EmptyState message="No exam results yet." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Subject</Th><Th>Exam</Th><Th>Marks</Th><Th>Grade</Th></tr></thead>
              <tbody>
                {results.map((r) => (
                  <tr key={r.id}>
                    <Td>{r.subjectName}</Td>
                    <Td>{EXAM_TYPES[r.examType] ?? r.examType}</Td>
                    <Td>{r.marks} / {r.maxMarks}</Td>
                    <Td>{r.grade}</Td>
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
